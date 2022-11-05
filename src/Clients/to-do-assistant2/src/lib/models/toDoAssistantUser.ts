import { User } from '../../../../shared2/models/user';

export class ToDoAssistantUser extends User {
	constructor(
		public email: string,
		public name: string,
		public language: string,
		public culture: string,
		public imageUri: string,
		public toDoNotificationsEnabled: boolean
	) {
		super(email, name, language, culture, imageUri);
	}
}
