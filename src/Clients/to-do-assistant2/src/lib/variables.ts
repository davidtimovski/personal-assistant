export default class Variables {
	static urls = {
		account: import.meta.env.VITE_ACCOUNT_URL,
		host: import.meta.env.VITE_HOST_URL,
		api: import.meta.env.VITE_API_URL,
		gateway: import.meta.env.VITE_GATEWAY_URL,
		defaultProfileImageUrl: import.meta.env.VITE_DEFAULT_PROFILE_IMAGE_URL
	};
	static debug = import.meta.env.DEV;
	static auth0ClientId = import.meta.env.VITE_AUTH0_CLIENT_ID;
}
