using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord;
using Discord.Commands;

namespace StupifyConsoleApp.Client
{
    public class MusicSearches
    {
        private ConcurrentDictionary<IGuildUser, MusicSearch> _musicSearches;

        public MusicSearches()
        {
            _musicSearches = new ConcurrentDictionary<IGuildUser, MusicSearch>();
        }

        public bool this[IGuildUser user] => _musicSearches.ContainsKey(user);
        public Uri this[IGuildUser user, int selection]
        {
            get
            {
                _musicSearches.TryRemove(user, out var musicSearch);
                return musicSearch.GetUrl(selection);
            }
        }

        public void AddSearch(IGuildUser user, Uri[] optionResults)
        {
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