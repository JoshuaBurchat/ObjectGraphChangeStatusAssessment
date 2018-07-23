using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment
{
    public interface IChangeAssessor<TKey>
    {
        IChangeTrackable<TKey>[] SetChangeStatus<T>(T source, T destination) where T : IChangeTrackable<TKey>;
        IChangeTrackable<TKey>[] SetChangeStatusList<T>(IList<T> list, IList<T> compareWith) where T : IChangeTrackable<TKey>;

    }
}
