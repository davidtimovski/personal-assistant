using Application.Contracts.ToDoAssistant.Lists.Models;

namespace Application.UnitTests.Builders
{
    public class ListBuilder
    {
        private string name;
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
                TasksText = tasksText
            };
        }

        public UpdateList BuildUpdateModel()
        {
            return new UpdateList
            {
                Name = name
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
