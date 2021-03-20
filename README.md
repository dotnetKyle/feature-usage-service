# feature-usage-service
A feature usage service that saves a description of feature usage per-user and also metadata about those features.

## Bulk Write Notes
Bulk Write Async doesn't seem to work correctly. Awaiting the task does not actually wait for it to finish. I recommend using BulkWrite instead since the thread will wait for it to finish.

## MongoDB Notes
Based on how the repo stores feature usage data in MongoDB, here are some queries that will help calculate reports for usage data and benchmarks:

> ### Usage Data Reports
>
> > Report to calculate the number of times each feature has been used.
> > ```
> > db.featureUsage.aggregate([
> >     { $unwind: "$usageData" },
> >     { $group: { _id: "$featureName",
> >         usageCount: { $sum: 1 }
> >       }
> >     },
> >     { $sort: { usageCount: -1 } }
> > ])
> > ```
> 
> 
> > Report to calculate the number of times a specific feature has been used.
> > ```
> > db.featureUsage.aggregate([
> >     { $match: { featureName:"Main Window|Long Running Service" } },
> >     { $unwind: "$usageData" },
> >     { $group: { _id: "$featureName",
> >         usageCount: { $sum: 1 }
> >       }
> >     }
> > ])
> > ```
> 
> 
> ### Benchmark Data Reports
>
> > Report to calculate the average benchmark for all features that are benchmarked.
> > 
> > ```
> > db.featureUsage.aggregate([
> >     { $project: {
> >             _id: 0,
> >             user: "$userName",
> >             featureName: "$featureName",
> >             benchmarkMs: "$usageData.benchmarkMs"
> >         }
> >     }, 
> >     { $match: {
> >             "benchmarkMs": { $exists: true, $ne: [] }
> >     }}, 
> >     { $unwind: { path: "$benchmarkMs" } }, 
> >     { $group: {
> >             _id: "$featureName",
> >             benchmarkAverage: {
> >                 $avg: "$benchmarkMs"
> >             }
> >     }}
> > ])
> > ```
> 
> > Report to calculate the average benchmark time for a specific feature.
> > ```
> > db.featureUsage.aggregate([
> >     { $match: { featureName:"Main Window|Long Running Service" } },
> >     { $project: {
> >         _id:0,
> >         user:"$userName",
> >         featureName:"$featureName",
> >         benchmarkMs:"$usageData.benchmarkMs"
> >       } 
> >     },
> >     { $unwind: "$benchmarkMs" },
> >     { $group: {
> >         _id: "$featureName",
> >         benchmarksAvg: { $avg: "$benchmarkMs" }
> >       }
> >     }
> > ])
> > ```