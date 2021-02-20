var nav = new Vue(
    {
        el: '#nav',

        data:
        {
            user: user,
            entries: []
        },

        methods:
        {
            get: async function () {
                const response = await fetch('http://localhost:5000/api/entries');
                const data = await response.json();
                this.entries = data;
            },

            logout: function () {
                let home = document.querySelector('#home-tab');
                let tab = new bootstrap.Tab(home);
                tab.show();

                user.logout();
            }
        }
    });