using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobs.Models.Accountant;

internal class DeletedEntity
{
    internal int UserId { get; set; }
    internal EntityType EntityType { get; set; }
    internal int EntityId { get; set; }
    internal DateTime DeletedDate { get; set; }
}
