﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.ObjectModel;

namespace OpenGlobe.Renderer
{
    public class UniformBlockMemberCollection : KeyedCollection<string, UniformBlockMember>
    {
        protected override string GetKeyForItem(UniformBlockMember item)
        {
            return item.Name;
        }
    }
}
