﻿using System.Collections.Generic;
using AutoMapper;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class ListWithSharingDetails : IMapFrom<ToDoList>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsOneTimeToggleDefault { get; set; }
        public ListSharingState SharingState { get; set; }
        public short? Order { get; set; }
        public List<TaskDto> Tasks { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ToDoList, ListWithSharingDetails>()
                .ForMember(x => x.SharingState, opt => opt.MapFrom<ListSharingStateResolver>())
                .ForMember(x => x.Order, opt => opt.MapFrom<ListOrderResolver>());
        }
    }
}
