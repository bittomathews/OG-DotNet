﻿using System.Collections.Generic;

using OGDotNet.Mappedtypes.Util.Db;

namespace OGDotNet.Mappedtypes.Master
{
    public class AbstractSearchResult<TDocument> //where TDocument extends Document
    {
        public Paging Paging { get; set; }
        public IList<TDocument> Documents { get; set; }
    }
}