//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

namespace Server.Engines.Reports
{
    using System;
    using System.Collections;
    
    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.QueueStatus.
    /// </summary>
    public class QueueStatusCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public QueueStatusCollection() : base()
        {
        }
        
        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.QueueStatus at a specific position in the QueueStatusCollection.
        /// </summary>
        public Server.Engines.Reports.QueueStatus this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.QueueStatus)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }
        
        /// <summary>
        /// Append a Server.Engines.Reports.QueueStatus entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.QueueStatus instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.QueueStatus value)
        {
            return this.List.Add(value);
        }
        
        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.QueueStatus instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.QueueStatus instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.QueueStatus instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.QueueStatus value)
        {
            return this.List.Contains(value);
        }
        
        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.QueueStatus instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.QueueStatus instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.QueueStatus instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.QueueStatus value)
        {
            return this.List.IndexOf(value);
        }
        
        /// <summary>
        /// Removes a specified Server.Engines.Reports.QueueStatus instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.QueueStatus instance to remove.</param>
        public void Remove(Server.Engines.Reports.QueueStatus value)
        {
            this.List.Remove(value);
        }
        
        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.QueueStatus instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.QueueStatus's enumerator.</returns>
        public new QueueStatusCollectionEnumerator GetEnumerator()
        {
            return new QueueStatusCollectionEnumerator(this);
        }
        
        /// <summary>
        /// Insert a Server.Engines.Reports.QueueStatus instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.QueueStatus instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.QueueStatus value)
        {
            this.List.Insert(index, value);
        }
        
        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.QueueStatus.
        /// </summary>
        public class QueueStatusCollectionEnumerator : System.Collections.IEnumerator
        {
            /// <summary>
            /// Current index
            /// </summary>
            private int _index;
            
            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.QueueStatus _currentElement;
            
            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private readonly QueueStatusCollection _collection;
            
            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal QueueStatusCollectionEnumerator(QueueStatusCollection collection)
            {
                this._index = -1;
                this._collection = collection;
            }
            
            /// <summary>
            /// Gets the Server.Engines.Reports.QueueStatus object in the enumerated QueueStatusCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.QueueStatus Current
            {
                get
                {
                    if (((this._index == -1) ||
                         (this._index >= this._collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return this._currentElement;
                    }
                }
            }
            
            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((this._index == -1) ||
                         (this._index >= this._collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return this._currentElement;
                    }
                }
            }
            
            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                this._index = -1;
                this._currentElement = null;
            }
            
            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((this._index <
                     (this._collection.Count - 1)))
                {
                    this._index = (this._index + 1);
                    this._currentElement = this._collection[this._index];
                    return true;
                }
                this._index = this._collection.Count;
                return false;
            }
        }
    }
}