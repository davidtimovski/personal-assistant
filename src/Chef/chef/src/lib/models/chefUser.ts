import { User } from '../../../../../Core/shared2/models/user';

export class ChefUser extends User {
	constructor(
		public email: string,
		public name: string,
		public language: string,
		public culture: string,
		public imageUri: string,
		public chefNotificationsEnabled: boolean,
		public imperialSystem: boolean
	) {
		super(email, name, language, culture, imageUri);
	}
}
