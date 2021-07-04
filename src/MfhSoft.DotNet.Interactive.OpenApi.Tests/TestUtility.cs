using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MfhSoft.DotNet.Interactive.OpenApi.Tests
{
    public static class AssertionExtensions
    {
        public static AndConstraint<GenericCollectionAssertions<KernelEvent>> NotContainErrors(
            this GenericCollectionAssertions<KernelEvent> should) =>
            should
                .NotContain(e => e is ErrorProduced)
                .And
                .NotContain(e => e is CommandFailed);

        public static AndWhichConstraint<ObjectAssertions, T> ContainSingle<T>(
            this GenericCollectionAssertions<KernelEvent> should,
            Func<T, bool> where = null)
            where T : KernelEvent
        {
            T subject;

            if (where is null)
            {
                should.ContainSingle(e => e is T);

                subject = should.Subject
                                .OfType<T>()
                                .Single();
            }
            else
            {
                should.ContainSingle(e => e is T && where((T)e));

                subject = should.Subject
                                .OfType<T>()
                                .Where(where)
                                .Single();
            }

            return new AndWhichConstraint<ObjectAssertions, T>(subject.Should(), subject);
        }        
    }

    public static class ObservableExtensions
    {
        public static SubscribedList<T> ToSubscribedList<T>(this IObservable<T> source)
        {
            return new SubscribedList<T>(source);
        }
    }

    public class SubscribedList<T> : IReadOnlyList<T>, IDisposable
    {
        private ImmutableArray<T> _list = ImmutableArray<T>.Empty;
        private readonly IDisposable _subscription;

        public SubscribedList(IObservable<T> source)
        {
            _subscription = source.Subscribe(x => { _list = _list.Add(x); });
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _list.Length;

        public T this[int index] => _list[index];

        public void Dispose() => _subscription.Dispose();
    }
}
