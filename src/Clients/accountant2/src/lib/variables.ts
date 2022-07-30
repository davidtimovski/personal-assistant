export default class Variables {
	static urls = {
		authority: import.meta.env.VITE_AUTHORITY_URL,
		host: import.meta.env.VITE_HOST_URL,
		api: import.meta.env.VITE_API_URL,
		gateway: import.meta.env.VITE_GATEWAY_URL,
		defaultProfileImageUrl: import.meta.env.VITE_DEFAULT_PROFILE_IMAGE_URL
	};
	static debug = import.meta.env.DEV;
}
