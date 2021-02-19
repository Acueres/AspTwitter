var nav = new Vue(
    {
        el: '#nav',

        data:
        {
            username: getUsername(),
            entries: [],
            logged: isLogged()
        },

        methods:
        {
            get: async function () {
                const response = await fetch('http://localhost:5000/api/entries');
                const data = await response.json();
                this.entries = data;
            },

            logout: function () {
                this.logged = false;
                this.username = '';
                localStorage.setItem('currentUser', JSON.stringify({ id: '', username: '', token: '', about: '' }));
            }
        }
    });

function isLogged() {
    let id = JSON.parse(localStorage.getItem('currentUser')).id;
    if (id == undefined) {
        return false;
    }
    return id !== '';
}

function getUsername() {
    let res = JSON.parse(localStorage.getItem('currentUser')).username;
    if (res == undefined) {
        return '';
    }

    return res;
}