var explore = new Vue({
    el: '#explore',

    data:
    {
        user: user,

        users: [],
        entries: [],
        selectedUser: null,
        selectedUserEntries: [],

        showProfile: false,
        searchUsers: true
    },

    components: {
        'tweet': tweetTemplate,
        'profile': profileTemplate
    },

    methods: {
        sendQuery: async function () {
            let entity = 'entries';
            if (this.searchUsers) {
                entity = 'users';
            }

            this.users = [];
            this.entries = [];

            let query = document.getElementById('search-bar').value;
            if (query == '') {
                return;
            }

            const response = await fetch(`http://localhost:5000/api/${entity}/search`, {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(query)
            })

            if (response.status != 400) {
                this.showProfile = false;

                if (this.searchUsers) {
                    this.users = await response.json();
                }
                else {
                    this.entries = await response.json();
                    this.entries.reverse();
                }
            }
        },

        openProfile: async function (user) {
            this.selectedUser = user;
            this.showProfile = true;

            let response = await fetch(`http://localhost:5000/api/users/${this.selectedUser.id}/entries`);
            this.selectedUserEntries = await response.json();
            this.selectedUserEntries.reverse();
        },

        deleteEntry: async function (id) {
            const response = await fetch(`http://localhost:5000/api/entries/${id}`, {
                method: 'DELETE',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + user.token
                }
            });

            if (response.status == 200) {
                entries.delete(id);
                user.deleteEntry(id);

                let index = this.entries.findIndex(x => x.id == id);
                if (index != -1) {
                    this.entries.splice(index, 1);
                }
            }
        },

        getAvatar: function (id) {
            return `http://localhost:5000/api/users/${id}/avatar`;
        }
    }
});