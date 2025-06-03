using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL;
internal class CallCollection : IEnumerable
{
    static readonly IEnumerable<BO.CallType> s_enums =
(Enum.GetValues(typeof(BO.CallType)) as IEnumerable<BO.CallType>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

}
internal class JobsCollection : IEnumerable
{
    static readonly IEnumerable<BO.Jobs> s_enums =
        (Enum.GetValues(typeof(BO.Jobs)) as IEnumerable<BO.Jobs>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class DistanceCollection : IEnumerable
{
    static readonly IEnumerable<BO.DistanceType> s_enums =
        (Enum.GetValues(typeof(BO.DistanceType)) as IEnumerable<BO.DistanceType>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}
