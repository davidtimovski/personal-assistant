const friendshipsTabButton = document.getElementById('friendships-tab-button');
const requestsTabButton = document.getElementById('requests-tab-button');
const declinedTabButton = document.getElementById('declined-tab-button');

const friendshipsTab = document.getElementById('friendships-tab');
const requestsTab = document.getElementById('requests-tab');
const declinedTab = document.getElementById('declined-tab');

friendshipsTabButton.addEventListener('click', () => {
    friendshipsTab.classList.add('visible');
    friendshipsTabButton.classList.add('selected');

    requestsTab.classList.remove('visible');
    requestsTabButton.classList.remove('selected');

    declinedTab.classList.remove('visible');
    declinedTabButton.classList.remove('selected');
});

requestsTabButton.addEventListener('click', () => {
    requestsTab.classList.add('visible');
    requestsTabButton.classList.add('selected');

    friendshipsTab.classList.remove('visible');
    friendshipsTabButton.classList.remove('selected');

    declinedTab.classList.remove('visible');
    declinedTabButton.classList.remove('selected');
});

declinedTabButton.addEventListener('click', () => {
    declinedTab.classList.add('visible');
    declinedTabButton.classList.add('selected');

    friendshipsTab.classList.remove('visible');
    friendshipsTabButton.classList.remove('selected');

    requestsTab.classList.remove('visible');
    requestsTabButton.classList.remove('selected');
});
