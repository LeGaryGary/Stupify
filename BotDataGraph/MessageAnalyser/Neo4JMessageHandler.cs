using System;
using System.Linq;
using System.Threading.Tasks;

using BotDataGraph.MessageAnalyser.Models;

using Neo4j.Driver.V1;

namespace BotDataGraph.MessageAnalyser
{

    public class Neo4JMessageHandler
    {
        private readonly IDriver driver;
        
        private readonly string startupTime;
        
        public Neo4JMessageHandler(Uri uri, IAuthToken authToken)
        {
            driver = GraphDatabase.Driver(uri, authToken);
            using (var session = driver.Session())
            {
                var time = DateTime.Now.ToString("s");

                startupTime = session.WriteTransaction(tx =>
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
            using (var session = driver.Session())
            {
                var messageNodeId = await AddMessageNodeAsync(message, session);

                var words = message.Content.ToLowerInvariant().ToWords();

                foreach (var word in words)
                {
                    var wordNodeId = await AddWordConnectionAsync(word, messageNodeId, session);

                    foreach (var symbol in word)
                    {
                        await AddSymbolConnectionAsync(symbol, wordNodeId, session);
                    }
                }
            }
        }

        // TODO: Make async
        private async Task<int> AddSymbolConnectionAsync(char symbol, int wordNodeId, ISession session)
        {
            return session.WriteTransaction(
                tx =>
                {
                    var reader = tx.Run(
                        "MATCH (w:Word) WHERE id(w) = $wordNodeId " +
                        "MERGE (s:Symbol {name: $symbol}) " +
                        "MERGE (w)-[r:HAS_SYMBOL]->(s) " +
                        "return id(s) ",
                        new {symbol, wordNodeId});

                    return reader.First()[0].As<int>();
                });
        }

        // TODO: Make async
        private async Task<int> AddWordConnectionAsync(string word, int messageNodeId, ISession session)
        {
            return session.WriteTransaction(
                tx =>
                {
                    var reader = tx.Run(
                        "MATCH (m:Message) WHERE id(m) = $messageNodeId " +
                        "MERGE (w:Word {name: $word}) " +
                        "MERGE (m)-[r:HAS_WORD]->(w) " +
                        "return id(w) ",
                        new { word, messageNodeId });

                    return reader.First()[0].As<int>();
                });
        }

        // TODO: Make async
        private async Task<int> AddMessageNodeAsync(Message message, ISession session)
        {
            var lastMessageNodeId = session.ReadTransaction(
                tx =>
                {
                    var reader = tx.Run(
                        "MATCH (c:Channel {channelId: $channelId})-[]-(message:Message) "
                        + "return id(message) "
                        + "ORDER BY message.time DESC "
                        + "LIMIT 1",
                        message.Parameters(startupTime));
                    try
                    {
                        return reader.First()[0].As<int>();
                    }
                    catch
                    {
                        return -1;
                    }
                });

            return session.WriteTransaction(tx =>
                {
                    var query1 = string.Empty;
                    var query2 = string.Empty;
                    if (lastMessageNodeId != -1)
                    {
                        query1 = "match (mOld:Message) WHERE ID(mOld) = $lastNodeId ";
                        query2 = "MERGE (mOld)-[nextMessage:NEXT_MESSAGE]->(m)";
                    }
                    // TODO: Separate out query to check for user and channel existence then create them separately
                    var reader = tx.Run(
                        query1 +
                        "MATCH (startup:Startup { time: $startupTime})" +
                        "MERGE (u:User {userId: $userId})" +
                        "MERGE (s:Server { serverId: $serverId})" +
                        "MERGE (channel:Channel {channelId: $channelId})" +
                        "MERGE (u)-[sent:SENT]->(m:Message { content: $content, time: $time})" + 
                        "MERGE (m)-[inStartup:IN_STARTUP]->(startup)" +
                        "MERGE (m)-[channelRelate:IN_CHANNEL]->(channel)" +
                        "MERGE (u)-[su:SERVER_USER]-(s)" + 
                        "MERGE (s)-[sc:SERVER_CHANNEL]->(channel)" +
                        query2 +
                        "return id(m)",
                        message.Parameters(startupTime, lastMessageNodeId));

                    return reader.Single()[0].As<int>();
                });
        }

        
    }
}
