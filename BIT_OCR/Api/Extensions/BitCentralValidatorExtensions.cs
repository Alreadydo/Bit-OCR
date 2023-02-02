using BiT.Central.Core.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BiT.Central.OCR.Api.Extensions
{
    public static class BitCentralValidatorExtensions
    {
        /// <summary>
        ///     Validate that the collection does not contain more than <paramref name="count"/> items.
        /// </summary>
        /// 
        /// <typeparam name="T">The type of object subject to the validation</typeparam>
        /// 
        /// <param name="self">The instance to extend</param>
        /// <param name="items">The items to validate</param>
        /// <param name="count">The (inclusive) maximum amount of items the collection can contain</param>
        /// 
        /// <exception cref="BitCentralException{string}">Thrown when the collection contains more than <paramref name="count"/> items</exception>
        /// 
        public static void NotMoreThan<T>(
             this BitCentralValidator self,
             IQueryable<T> items,
             int count)
        {
            if (items.Count() > count)
            {
                self.Logger.LogWarning("Found more than {@Count} resources based on the given identifier", count);
                var error = self.ErrorFactory.Conflict($"Found more than {count} resources based on the given identifier");
                throw new BitCentralException<string>(error);
            }
        }
    }
}
