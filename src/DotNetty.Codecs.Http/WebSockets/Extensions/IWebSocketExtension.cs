﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Http.WebSockets.Extensions
{
    public interface IWebSocketExtension
    {
        /// <summary>
        /// The reserved bit value to ensure that no other extension should interfere.
        /// </summary>
        int Rsv { get; }

        WebSocketExtensionEncoder NewExtensionEncoder();

        WebSocketExtensionDecoder NewExtensionDecoder();
    }

    public static class WebSocketRsv
    {
        public const int Rsv1 = 0x04;
        public const int Rsv2 = 0x02;
        public const int Rsv3 = 0x01;
    }
}
