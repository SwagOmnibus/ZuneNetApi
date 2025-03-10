﻿using Atom.Xml;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Zune.Net.Helpers;
using Zune.Xml.Catalog;

namespace Zune.Net.Catalog.Controllers.Music
{
    [Route("/music/features/")]
    [Produces(Atom.Constants.ATOM_MIMETYPE)]
    public class FeaturesController : Controller
    {
        [HttpGet, Route("featured/albums")]
        public async Task<ActionResult<Feed<Album>>> FeaturedAlbums()
        {
            var dz_albums = await Deezer.GetChartDZArtists();
            DateTime updated = DateTime.Now;

            Feed<Album> feed = new()
            {
                Id = "albums",
                Title = "Albums",
                Author = Deezer.DZ_AUTHOR,
                Updated = updated
            };

            foreach (var dz_album in dz_albums)
            {
                var mb_release = Deezer.GetMBReleaseByDZAlbum(dz_album);
                if (mb_release == null)
                    continue;

                var album = MusicBrainz.MBReleaseToAlbum(mb_release, updated: updated);
                album.Explicit = dz_album.Value<bool>("explicit_lyrics");

                feed.Entries.Add(album);
            }

            return feed;
        }

        [HttpGet, Route("features")]
        public async Task<ActionResult<Feed<Feature>>> Features()
        {
            throw new NotImplementedException();
        }
    }
}
