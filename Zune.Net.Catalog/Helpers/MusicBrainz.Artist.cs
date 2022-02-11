﻿using Atom.Xml;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using System;
using Zune.Xml.Catalog;

namespace Zune.Net.Catalog.Helpers
{
    public partial class MusicBrainz
    {
        private static readonly Guid ARTIST_VARIOUSARTISTS = new("89ad4ac3-39f7-470e-963a-56509c546377");
        private static readonly Guid ARTIST_NOARTIST = new("eec63d3c-3b81-4ad4-b1e4-7c147d4d2b61");

        public Feed<Artist> SearchArtists(string query, string requestPath)
        {
            var results = _query.FindAllArtists(query, simple: true);
            var updated = DateTime.Now;
            Feed<Artist> feed = new()
            {
                Id = "artists",
                Title = "artists",
                Links = { new(requestPath) },
                Updated = updated,
                Entries = new()
            };

            // Add results to feed
            foreach (var result in results)
            {
                if (feed.Entries.Count == 40) break;

                var mb_rec = result.Item;
                feed.Entries.Add(MBArtistToArtist(mb_rec, updated: updated));
            }

            return feed;
        }

        public Artist GetArtistByMBID(Guid mbid)
        {
            var mb_artist = _query.LookupArtist(mbid, Include.Releases | Include.Recordings);
            return MBArtistToArtist(mb_artist);
        }

        public Feed<Track> GetArtistTracksByMBID(Guid mbid, string requestPath, int chunkSize)
        {
            var results = _query.BrowseAllArtistRecordings(mbid, pageSize: chunkSize, inc: Include.ArtistCredits);
            var updated = DateTime.Now;
            Feed<Track> feed = new()
            {
                Id = mbid.ToString(),
                Title = "tracks",
                Links = { new(requestPath) },
                Updated = updated,
                Entries = new(chunkSize)
            };

            // Add results to feed
            foreach (var mb_rec in results)
            {
                if (feed.Entries.Count == chunkSize) break;

                feed.Entries.Add(MBRecordingToTrack(mb_rec, updated: updated, includeRights: true));
            }

            return feed;
        }

        public Feed<Album> GetArtistAlbumsByMBID(Guid mbid, string requestPath)
        {
            var results = _query.BrowseAllArtistReleases(mbid, inc: Include.ArtistCredits);
            var updated = DateTime.Now;
            Feed<Album> feed = new()
            {
                Id = mbid.ToString(),
                Title = "albums",
                Links = { new(requestPath) },
                Updated = updated,
                Entries = new()
            };

            // Add results to feed
            foreach (var mb_release in results)
            {
                //if (feed.Entries.Count == chunkSize) break;

                feed.Entries.Add(MBReleaseToAlbum(mb_release, updated: updated));
            }

            return feed;
        }


        public Artist MBArtistToArtist(IArtist mb_artist, DateTime? updated = null)
        {
            updated ??= DateTime.Now;
            Artist artist = new()
            {
                Id = mb_artist.Id.ToString(),
                Title = mb_artist.Name,
                SortTitle = mb_artist.SortName,
                IsVariousArtist = mb_artist.Id == ARTIST_VARIOUSARTISTS,
                BiographyLink = "https://bing.com",
                Updated = updated.Value,
            };

            if (mb_artist.Genres != null && mb_artist.Genres.Count > 0)
            {
                var mb_genre = mb_artist.Genres[0];
                if (mb_genre != null)
                    artist.PrimaryGenre = MBGenreToGenre(mb_genre);

                artist.Genres ??= new();
                foreach (var genre in mb_artist.Genres)
                    artist.Genres.Add(MBGenreToGenre(genre));
            }

            return artist;
        }

        public MiniArtist MBNameCreditToMiniArtist(INameCredit mb_credit)
        {
            return new()
            {
                Id = mb_credit.Artist.Id,
                Title = mb_credit.Name
            };
        }

        public MiniArtist MBArtistToMiniArtist(IArtist mb_artist)
        {
            return new()
            {
                Id = mb_artist.Id,
                Title = mb_artist.Name
            };
        }
    }
}
