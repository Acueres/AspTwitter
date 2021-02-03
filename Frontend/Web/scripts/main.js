var main = new Vue(
    {
        el: '#main',
        data:
        {
            entries: []
        },

        methods:
        {
            get: async function()
                {
                    const response = await fetch('http://localhost:5000/api/entries');
                    const data = await response.json();
                    this.entries = data;
                }
        }
    });

async function post()
{
    const currentUser = JSON.parse(localStorage.getItem('currentUser'));

    const post = await fetch('http://localhost:5000/api/entries', {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + currentUser.token},
        body: JSON.stringify({
            AuthorId: currentUser.id,
            Text: 'General Kenobi'
            })
        });
}

main.get();