using System;
using System.Collections.Concurrent;
using Discord;

namespace StupifyConsoleApp.Client.Audio
{
    public class MusicSearches
    {
        private readonly ConcurrentDictionary<IGuildUser, MusicSearch> _musicSearches;

        public MusicSearches()
        {
            _musicSearches = new ConcurrentDictionary<IGuildUser, MusicSearch>();
        }
    
        public Uri this[IGuildUser user, int selection]
        {
            get
            {
                _musicSearches.TryRemove(user, out var musicSearch);
                return musicSearch.GetUrl(selection);
            }
        }

        public bool UserHasSearch(IGuildUser user) => _musicSearches.ContainsKey(user);

        public void AddSearch(IGuildUser user, Uri[] optionResults)
        {
            _musicSearches.TryRemove(user, out var _);
            _musicSearches.TryAdd(user, new MusicSearch(optionResults));
        }
    }

    public class MusicSearch
    {
        private readonly Uri[] _optionResults;

        public MusicSearch(Uri[] optionResults)
        {
            _optionResults = optionResults;
        }

        public Uri GetUrl(int selection)
        {
            return _optionResults[selection];
        }
    }

}