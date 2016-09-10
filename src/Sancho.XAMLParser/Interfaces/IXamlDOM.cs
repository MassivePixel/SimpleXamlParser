// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;

namespace Sancho.XAMLParser.Interfaces
{
    public interface IXamlDOM
    {
        Action<string> Logger { get; set; }

        void AddNode(XamlNode root);
    }
}