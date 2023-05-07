using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobs.Models.Accountant;

internal class DeletedEntity
{
    public int UserId { get; set; }
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    public DateTime DeletedDate { get; set; }
}
