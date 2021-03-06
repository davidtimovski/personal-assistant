﻿using System.Collections.Generic;
using AutoMapper;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class TaskForUpdate : IMapFrom<ToDoTask>
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOneTime { get; set; }
        public bool IsPrivate { get; set; }
        public int? AssignedToUserId { get; set; }
        public bool IsInSharedList { get; set; }
        public short Order { get; set; }
        public List<string> Recipes { get; set; } = new List<string>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoTask, TaskForUpdate>()
                .ForMember(x => x.IsPrivate, opt => opt.MapFrom<IsPrivateResolver>())
                .ForMember(x => x.IsInSharedList, opt => opt.Ignore())
                .ForMember(x => x.Recipes, opt => opt.Ignore());
        }
    }
}
