using Xunit;

// Formati.LinguaSchermo è uno stato condiviso: i test che la cambiano non devono girare
// in parallelo con quelli che la leggono. I test sono pochi e veloci, quindi si disattiva
// il parallelismo per tutto il progetto.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
