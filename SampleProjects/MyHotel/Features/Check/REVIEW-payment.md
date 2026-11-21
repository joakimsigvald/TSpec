# Room price and payment — review

Re-check: the last review's eight proposals are made.

1. **`HasAllowedBedCount` is now called once, from `EnsureAllowed`, and only restates its body**
   (CODING-STANDARD-BACKEND.md 4d). Inline it, so the two guards read alike —
   `if (room.BedCount is < BedCountLimits.Min or > BedCountLimits.Max)` above
   `throw new InvalidBedCount(room.BedCount);` — and delete `HasAllowedBedCount`.
