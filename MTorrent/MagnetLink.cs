// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using MTorrent.Helpers;

namespace MTorrent
{
    public static class MagnetLink
    {
        public static string Create(TorrentInfo torrentInfo, bool includeTrackers = true)
        {
            if (torrentInfo is null)
                throw new ArgumentNullException(nameof(torrentInfo));

            int size = 60 +
                (includeTrackers ? 150 : 0) +
                (string.IsNullOrWhiteSpace(torrentInfo.DisplayName) ? 0 : torrentInfo.DisplayName.Length * 2);

            StringBuilder sb = new StringBuilder(size);

            sb.Append("magnet:?xt=urn:btih:");

            Hex.Encode(torrentInfo.InfoHashV2 ?? torrentInfo.InfoHashV1, sb);

            if (!string.IsNullOrWhiteSpace(torrentInfo.DisplayName))
            {
                sb.Append("&dn=");
                sb.Append(HttpUtility.UrlEncode(torrentInfo.DisplayName));
            }

            if (includeTrackers)
            {
                foreach (var tracker in torrentInfo.Trackers)
                {
                    sb.Append("&tr=");
                    sb.Append(HttpUtility.UrlEncode(tracker));
                }
            }

            return sb.ToString();
        }

        public static bool TryParse(ReadOnlySpan<char> link, out TorrentInfo torrentInfo)
        {
            torrentInfo = null;

            // "magnet:?xt=urn:btih:" = 20 chars
            if (link.Length < 20 + 2 * 20)
                return false;

            if (!link.StartsWith("magnet:?", StringComparison.OrdinalIgnoreCase))
                return false;

            link = link.Slice(8);

            byte[] infoHash = null;
            string displayName = null;
            List<string> trackers = new List<string>();

            try
            {
                while (link.Length > 0)
                {
                    int tagEndIdx = link.IndexOf('=');
                    if (tagEndIdx < 1 || tagEndIdx > 32)
                        return false;

                    Span<char> tag = stackalloc char[tagEndIdx * 2];
                    int tagLen = link.Slice(0, tagEndIdx).ToLowerInvariant(tag);
                    ReadOnlySpan<char> roTag = tag.Slice(0, tagLen);
                    link = link.Slice(tagEndIdx + 1);

                    int endIndex = link.IndexOf('&');
                    ReadOnlySpan<char> value = endIndex == -1
                        ? link
                        : link.Slice(0, endIndex);

                    link = endIndex == -1
                        ? ReadOnlySpan<char>.Empty
                        : link.Slice(endIndex + 1);

                    if (roTag.SequenceEqual("xt"))
                    {
                        if (value.Length < 9 + 2 * 20)
                            return false;

                        if (!value.StartsWith("urn:btih:", StringComparison.OrdinalIgnoreCase) &&
                            !value.StartsWith("urn:sha1:", StringComparison.OrdinalIgnoreCase))
                            return false;

                        if (!Hex.TryParse(value.Slice(9), out infoHash))
                            return false;
                    }
                    else if (roTag.SequenceEqual("dn"))
                    {
                        displayName = HttpUtility.UrlDecode(value.ToString());
                    }
                    else if (roTag.SequenceEqual("tr"))
                    {
                        trackers.Add(HttpUtility.UrlDecode(value.ToString()));
                    }
                    else
                    {
                        // ToDo - Log unknown params
                    }
                }
            }
            catch
            {
                return false;
            }

            if (infoHash is null)
                return false;

            torrentInfo = new TorrentInfo()
            {
                DisplayName = displayName
            };
            torrentInfo.Trackers.AddRange(trackers);
            return true;
        }
    }
}
