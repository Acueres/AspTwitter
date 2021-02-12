var home = new Vue(
    {
        el: '#home',

        data:
        {
            logged: isLogged(),
            entries: [],
            text: ''
        },

        created: async function () {
            const id = JSON.parse(localStorage.getItem('currentUser')).id;

            if (id !== undefined && id != '') {
                const response = await fetch(`http://localhost:5000/api/users/${id}/entries`);
                const data = await response.json();
                this.entries = data;
                this.entries.reverse();
            }
            else {
                const response = await fetch(`http://localhost:5000/api/entries`);
                const data = await response.json();
                this.entries = data;
                this.entries.reverse();
            }
        },

        methods:
        {
            post: async function () {
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
                        authorId: currentUser.id,
                        text: this.text
                    })
                });

                if (response.status === 200) {
                    this.entries.splice(0, 0, {
                        author: currentUser,
                        authorId: currentUser.id,
                        text: String(this.text)
                    });

                    this.text = '';
                    document.getElementById('post').value = '';

                    let toast = new bootstrap.Toast(document.getElementById('successToast'));
                    toast.show();
                }
            }
        },

        computed:
        {
            charactersLeft: function () {
                if (this.text.length == 0) {
                    return null;
                }

                return this.text.length + '/' + '256';
            }
        }
    });