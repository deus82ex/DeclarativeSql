﻿using System;
using System.Collections.Generic;
using System.Linq;



namespace DeclarativeSql.Internals
{
    /// <summary>
    /// Provides <see cref="IEnumerable{T}"/> extension methods
    /// </summary>
    internal static class EnumerableExtensions
    {
        #region WithIndex
        /// <summary>
        /// Add an index to the element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<(T element, int index)> WithIndex<T>(this IEnumerable<T> source)
            => source.Select((x, i) => (x, i));
        #endregion


        #region Materialize
        /// <summary>
        /// Return array that is materialized if source is deferred, otherwise return itself without do nothing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="nullToEmpty"></param>
        public static IEnumerable<T> Materialize<T>(this IEnumerable<T> source, bool nullToEmpty = true)
        {
            if (source == null)
            {
                if (nullToEmpty)
                    return Enumerable.Empty<T>();
                throw new ArgumentNullException("source is null.");
            }
            if (source is ICollection<T>) return source;
            if (source is IReadOnlyCollection<T>) return source;
            return source.ToArray();
        }
        #endregion


        #region Buffer
        /// <summary>
        /// Generates a sequence of non-overlapping adjacent buffers over the source sequence.
        /// </summary>
        /// <typeparam name="TSource">Source sequence element type.</typeparam>
        /// <param name="source">Source sequence.</param>
        /// <param name="count">Number of elements for allocated buffers.</param>
        /// <returns>Sequence of buffers containing source sequence elements.</returns>
        public static IEnumerable<IList<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));

            return BufferCore(source, count, count);
        }


        /// <summary>
        /// Generates a sequence of buffers over the source sequence, with specified length and possible overlap.
        /// </summary>
        /// <typeparam name="TSource">Source sequence element type.</typeparam>
        /// <param name="source">Source sequence.</param>
        /// <param name="count">Number of elements for allocated buffers.</param>
        /// <param name="skip">Number of elements to skip between the start of consecutive buffers.</param>
        /// <returns>Sequence of buffers containing source sequence elements.</returns>
        public static IEnumerable<IList<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, int count, int skip)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (skip <= 0) throw new ArgumentOutOfRangeException(nameof(skip));

            return BufferCore(source, count, skip);
        }


        private static IEnumerable<IList<TSource>> BufferCore<TSource>(IEnumerable<TSource> source, int count, int skip)
        {
            var buffers = new Queue<IList<TSource>>();

            var i = 0;
            foreach (var item in source)
            {
                if (i % skip == 0)
                    buffers.Enqueue(new List<TSource>(count));

                foreach (var buffer in buffers)
                    buffer.Add(item);

                if (buffers.Count > 0 && buffers.Peek().Count == count)
                    yield return buffers.Dequeue();

                i++;
            }

            while (buffers.Count > 0)
                yield return buffers.Dequeue();
        }
        #endregion
    }
}
