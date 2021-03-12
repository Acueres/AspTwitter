var nav = new Vue(
    {
        el: '#nav',

        data:
        {
            user: user,
        },

        methods:
        {
            logout: function () {
                let home = document.querySelector('#home-tab');
                let tab = new bootstrap.Tab(home);
                tab.show();

                login.clear();
                user.clear();
            }
        }
    });