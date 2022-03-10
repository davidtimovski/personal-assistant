﻿using System;

namespace Domain.Entities.Accountant;

public class DeletedEntity
{
    public int UserId { get; set; }
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    public DateTime DeletedDate { get; set; }
}
