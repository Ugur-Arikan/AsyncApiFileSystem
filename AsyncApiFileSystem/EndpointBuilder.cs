namespace AsyncApiFileSystem;

internal class EndpointBuilder<Job, In, IdFact, Id>
    where Job : IJob<Id, In>
    where Id : IComparable<Id>, IEquatable<Id>
    where IdFact : IIdFactory<Id>
{
    // data
    readonly ISession<Job, In, IdFact, Id> Session;

    
    // methods
    //public Func<ISession<Job, In, IdFact, Id>, >
}
