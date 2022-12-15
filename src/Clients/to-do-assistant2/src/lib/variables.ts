export default class Variables {
	static urls = {
		account: import.meta.env.VITE_ACCOUNT_URL,
		host: import.meta.env.VITE_HOST_URL,
		gateway: import.meta.env.VITE_GATEWAY_URL,
		api: import.meta.env.VITE_GATEWAY_URL + '/todo/api',
		defaultProfileImageUrl:
			'https://res.cloudinary.com/personalassistant/w_80,h_80,c_limit/production/defaults/sfmqac.webp'
	};
	static debug = import.meta.env.DEV;
	static auth0Domain = import.meta.env.VITE_AUTH0_DOMAIN;
	static auth0ClientId = import.meta.env.VITE_AUTH0_CLIENT_ID;
}
