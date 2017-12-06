using System.Collections.Generic;
using System.Linq;

namespace Mapzen.VectorData
{
    /// <summary>
    /// Thread safe Tile Cache (on Feature Collection data)
    /// </summary>
    /// <remarks>
    /// Stores the feature collections data by tile address, this Cache is thread safe.
    /// Capacity is in number of tiles.
    public class TileCache
    {
        // An entry in the FIFO cache
        private class TileCacheEntry
        {
            public TileAddress address;
            public IEnumerable<FeatureCollection> featureCollections;
        }

        private Queue<TileCacheEntry> cache;

        // The capacity (in number of tiles)
        private int capacity;

        /// <summary>
        /// Construct a Tile cache with a capacity given in number of tiles.
        /// </summary>
        /// <param name="capacity">The number of tiles this cache can contain.</param>
        public TileCache(int capacity)
        {
            this.cache = new Queue<TileCacheEntry>();
            this.capacity = capacity;
        }

        /// <summary>
        /// Gets a list of feature collection stored in the cache for a specified address.
        /// May return null.
        /// </summary>
        /// <param name="address">The tile address.</param>
        /// <returns><c>null</c>, if there is no data stored for the tile address.</returns>
        public IEnumerable<FeatureCollection> Get(TileAddress address)
        {
            IEnumerable<FeatureCollection> featureCollections = null;

            lock (cache)
            {
                if (this.cache.Count > 0)
                {
                    var cacheEntry = this.cache.FirstOrDefault(entry => entry.address.Equals(address));
                    if (cacheEntry != null)
                    {
                        featureCollections = cacheEntry.featureCollections;
                    }
                }
            }

            return featureCollections;
        }

        /// <summary>
        /// Clears the tile cache.
        /// </summary>
        public void Clear()
        {
            lock (cache)
            {
                cache.Clear();
            }
        }

        /// <summary>
        /// Adds a new entry in the tile cache, clears an entry if max capacity is reached.
        /// </summary>
        /// <param name="address">The tile address.</param>
        /// <param name="featureCollections">The list of feature collections to store.</param>
        public void Add(TileAddress address, IEnumerable<FeatureCollection> featureCollections)
        {
            lock (cache)
            {
                TileCacheEntry cacheEntry = null;

                if (cache.Count > 0)
                {
                    cacheEntry = this.cache.FirstOrDefault(entry => entry.address.Equals(address));
                }

                // No entry for this address, queue a new one and
                // free the cache if maximum capacity is reached
                if (cacheEntry == null)
                {
                    var entry = new TileCacheEntry();
                    entry.address = address;
                    entry.featureCollections = featureCollections;

                    cache.Enqueue(entry);

                    if (cache.Count > capacity)
                    {
                        cache.Dequeue();
                    }
                }
                else
                {
                    // Update the entry with the new data if necessary
                    cacheEntry.featureCollections = featureCollections;
                }
            }
        }
    }
}
