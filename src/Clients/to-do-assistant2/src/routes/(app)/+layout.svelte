<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount } from 'svelte/internal';
	import type { User } from 'oidc-client';

	import { AuthService } from '../../../../shared2/services/authService';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { locale } from '$lib/localization/i18n';
	import { isOnline, loggedInUser, syncStatus, lists } from '$lib/stores';
	import { SyncEvents } from '$lib/models/syncEvents';
	import { ListsService } from '$lib/services/listsService';
	import { SignalRClient } from '$lib/utils/signalRClient';
	import { List, Task } from '$lib/models/entities';
	import { SharingState } from '$lib/models/viewmodels/sharingState';

	import Alert from '$lib/components/Alert.svelte';

	let listsService: ListsService;
	let signalRClient: SignalRClient;
	let user: User | null = null;

	function generateComputedLists(allLists: List[]) {
		const allTasks: Task[] = allLists
			.filter((x) => !x.isArchived && !x.computedListType)
			.reduce((a: Task[], b: List) => {
				return a.concat(b.tasks);
			}, []);

		const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);
		const highPriorityList = allLists.find((x) => x.computedListType);
		if (uncompletedHighPriorityTasks.length > 0) {
			if (highPriorityList) {
				highPriorityList.tasks = uncompletedHighPriorityTasks;
			} else {
				allLists.push(
					new List(
						0,
						null,
						null,
						false,
						false,
						SharingState.NotShared,
						0,
						false,
						ListsService.highPriorityComputedListMoniker,
						uncompletedHighPriorityTasks,
						null
					)
				);
			}
		} else if (highPriorityList) {
			const index = allLists.indexOf(highPriorityList);
			allLists.splice(index, 1);
		}
	}

	async function sync(localStorage: LocalStorageUtil) {
		syncStatus.set(SyncEvents.SyncStarted);

		const l = await listsService.getAll();

		const highPriorityListsEnabled = localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled);
		if (highPriorityListsEnabled) {
			generateComputedLists(l);
		}

		lists.set(l);

		syncStatus.set(SyncEvents.SyncFinished);
	}

	onMount(() => {
		const localStorage = new LocalStorageUtil();
		locale.set(localStorage.get('language'));

		listsService = new ListsService();
		signalRClient = new SignalRClient();

		loggedInUser.subscribe(async (value) => {
			if (!value) {
				return;
			}

			user = value;

			await sync(localStorage);

			signalRClient.initialize(user.access_token, parseInt(user.profile.sub, 10));
		});

		new AuthService('to-do-assistant2', window).login();

		isOnline.set(navigator.onLine);
		window.addEventListener('online', () => {
			isOnline.set(true);
		});
		window.addEventListener('offline', () => {
			isOnline.set(false);
		});

		syncStatus.subscribe((value) => {
			if (value === SyncEvents.ReSync) {
				sync(localStorage);
			}
		});
	});
</script>

<main>
	<div />
	<div class="center">
		<slot />
		<Alert />
	</div>
	<div />
</main>
