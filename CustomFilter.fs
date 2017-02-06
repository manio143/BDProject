module Filters
    type Filter() =
        static member Str (o:System.Object) = 
            if o.GetType().Equals(typeof<decimal>) then (o :?> decimal).ToString("F6", System.Globalization.CultureInfo.InvariantCulture)
            else string o
