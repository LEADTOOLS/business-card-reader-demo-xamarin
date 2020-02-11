// *************************************************************
// Copyright (c) 1991-2019 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BCReaderDemo
{
   public class Grouping<K, T> : ObservableCollection<T>
   {
      public K Key { get; private set; }

      public Grouping(K key, IEnumerable<T> items)
      {
         Key = key;
         foreach (var item in items)
            this.Items.Add(item);
      }
   }
}
