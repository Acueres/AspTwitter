var home = new Vue(
    {
        el: '#home',

        data:
        {
            appUser: appUser,
            entries: entries,
            text: ''
        },

        components: {
            'tweet': tweetTemplate,
            'user-info': userInfoTemplate
        },

        methods:
        {
            post: async function () {
                const response = await fetch(server + 'api/entries', {
                    method: 'POST',
                    credentials: 'omit',
                    redirect: 'follow',
                    cache: 'no-cache',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + appUser.token
                    },
                    body: JSON.stringify({
                        authorId: appUser.id,
                        text: this.text
                    })
                });

                if (response.status === 200) {
                    const entryId = parseInt(await response.json());

                    let entry = {
                        id: entryId,
                        author: Object.assign({}, appUser),
                        authorId: appUser.id,
                        text: String(this.text),
                        timestamp: new Date().toUTCString()
                    };

                    entries.add(entry);
                    appUser.addEntry(entry);

                    this.text = '';
                    document.getElementById('post').value = '';
                }
            }
        }
    });