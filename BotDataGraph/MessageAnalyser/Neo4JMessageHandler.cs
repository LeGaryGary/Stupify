using System;
using System.Linq;
using System.Threading.Tasks;

using BotDataGraph.MessageAnalyser.Models;

using Neo4j.Driver.V1;

namespace BotDataGraph.MessageAnalyser
{
    using System.Collections.Generic;

    public class Neo4JMessageHandler
    {
        private readonly IDriver driver;
        
        private readonly string startupTime;
        
        public Neo4JMessageHandler(Uri uri, IAuthToken authToken)
        {
            this.driver = GraphDatabase.Driver(uri, authToken);
            using (var session = this.driver.Session())
            {
                var time = DateTime.Now.ToString("s");

                this.startupTime = session.WriteTransaction(tx =>
                    {
                        var result = tx.Run(
                            "CREATE (a:Startup) SET a.time = $time RETURN a.time",
                        new {time});

                    return result.Single()[0].As<string>();
                    });
            }
        }
        
        public async Task Handle(Message message)
        {
            using (var session = this.driver.Session())
            {
                var messageNodeId = await AddMessageNode(message, session);

                //var words = message.Content.Split(' ');
                //foreach (var word in words)
                //{
                //    await
                //}
            }
        }

        private async Task<int> AddMessageNode(Message message, ISession session)
        {
            var lastMessageNodeId = session.ReadTransaction(
                tx =>
                {

                    var reader = tx.Run(
                        "MATCH (c:Channel {channelId: $channelId})-[]-(message:Message) "
                        + "return id(message) "
                        + "ORDER BY message.time DESC "
                        + "LIMIT 1",
                        message.Parameters(this.startupTime));

                    return reader.Single()[0].As<int>();
                });

            return session.WriteTransaction(
                tx =>
                {
                    // TODO: Separate out query to check for user and channel existence then create them separately
                    var reader = tx.Run(
                        "match (mOld:Message) WHERE ID(mOld) = $lastNodeId " +
                        "MATCH (startup:Startup { time: $startupTime})" +
                        "MERGE (u:User {userId: $userId})" +
                        "MERGE (s:Server { serverId: $serverId})" +
                        "MERGE (channel:Channel {channelId: $channelId})" +
                        "MERGE (u)-[sent:SENT]->(m:Message { content: $content, time: $time})" + 
                        "MERGE (m)-[inStartup:IN_STARTUP]->(startup)" +
                        "MERGE (m)-[channelRelate:IN_CHANNEL]->(channel)" +
                        "MERGE (u)-[su:SERVER_USER]-(s)" + 
                        "MERGE (s)-[sc:SERVER_CHANNEL]->(channel)" +
                        "MERGE (mOld)-[nextMessage:NEXT_MESSAGE]->(m)" +
                        "return id(m)",
                        message.Parameters(this.startupTime, lastMessageNodeId));

                    return reader.Single()[0].As<int>();
                });
        }
    }
}
