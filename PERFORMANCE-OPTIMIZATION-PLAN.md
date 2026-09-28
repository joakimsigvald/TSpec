# Pipeline sharing (4.0)

The Facts of a class that read the outcome before anything else share one run, set up in the
constructor and acted on once, and check the outcome one after the other in declared order. A Fact
that sets up or mentions a value first, and each Theory row, runs alone. A separate run, or parallel
execution, takes a separate class. `TSPEC_SHARING=off` turns sharing off, to compare.

Target: MyHotel.Spec ≥10–20% faster, preferably >50%, with <1% overhead where nothing is shared.

Steps are done in order; delete one when it lands.

4. **Measure MyHotel.Spec** with and without `TSPEC_SHARING=off`.
6. **Docs and release note.** The README says "the entire test pipeline is built and disposed for
   each test method"; the agent reference says "at most once per test method" and "every test a
   fresh `HttpClient`". The 4.0 release note names what breaks: a Fact that consumes or changes what
   the next reads — a stream, a lazy enumerable, the subject.
