using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;

namespace PersonalAssistant.Application.UnitTests.Builders
{
    public class ListBuilder
    {
        private string name;
        private bool isOneTimeToggleDefault;
        private string tasksText;

        public ListBuilder()
        {
            name = "Dummy name";
        }

        public ListBuilder WithName(string newName)
        {
            name = newName;
            return this;
        }

        public ListBuilder WithIsOneTimeToggleDefault(bool newIsOneTimeToggleDefault)
        {
            isOneTimeToggleDefault = newIsOneTimeToggleDefault;
            return this;
        }

        public ListBuilder WithTasksText(string newTasksText)
        {
            tasksText = newTasksText;
            return this;
        }

        public CreateList BuildCreateModel()
        {
            return new CreateList
            {
                Name = name,
                IsOneTimeToggleDefault = isOneTimeToggleDefault,
                TasksText = tasksText
            };
        }

        public UpdateList BuildUpdateModel()
        {
            return new UpdateList
            {
                Name = name,
                IsOneTimeToggleDefault = isOneTimeToggleDefault
            };
        }

        public UpdateSharedList BuildUpdateSharedModel()
        {
            return new UpdateSharedList();
        }

        public CopyList BuildCopyModel()
        {
            return new CopyList
            {
                Name = name
            };
        }
    }
}
