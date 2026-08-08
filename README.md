# Useful Async Algorithms

A collection of practical async algorithms and patterns for .NET.  
These techniques are especially helpful in **event-driven systems** where handling bursts of events, concurrency, and load distribution are key.

Requires .NET 9.

## Articles (Polish)

Code samples for the blog series:  
https://beniaminlenarcik.pl/series/uzyteczne-algorytmy-w-systemach-opartych-na-zdarzeniach

## Patterns

| Pattern | Idea |
|---------|------|
| **Debounce** | Emit at most one event per key within a time window (e.g. file-change storms). |
| **Jitter** | Add random delay so many clients do not hit a service at the same instant. |
| **Double-checked locking** | Cheap read path with a single async initialization / refresh under concurrency. |
| **Leaky bucket** | Accept bursts into a queue; release work downstream at a constant rate. |

Examples are **learning-oriented** demos, not production-hardened libraries.

## Purpose

Ready-to-run implementations and learning examples of algorithms that are easy to overlook day-to-day, but improve stability under load and bursts of events.

## License

MIT
