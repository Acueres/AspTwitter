var main = new Vue(
    {
        el: '#main',
        data:
        {
            username: JSON.parse(localStorage.getItem('currentUser')).username,
            entries: [],
            logged: JSON.parse(localStorage.getItem('currentUser')).id !== ''
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
                localStorage.setItem('currentUser', JSON.stringify({ id: '', username: '', token: '' }));
                location.reload();
            }
        }
    });

async function post() {
    const currentUser = JSON.parse(localStorage.getItem('currentUser'));

    const response = await fetch('http://localhost:5000/api/entries', {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + currentUser.token
        },
        body: JSON.stringify({
            AuthorId: currentUser.id,
            Text: 'General Kenobi'
        })
    });
}

main.get();