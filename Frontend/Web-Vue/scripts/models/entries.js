class Entries {
    data = [];
    part = 0;

    constructor() {
        this.load();
    }

    async load() {
        const response = await fetch(server + `api/entries/partial/${this.part}`, {
            headers: {
                credentials: 'omit',
                'ApiKey': apiKey
            }
        });

        if (response.status == 200) {
            const data = await response.json();

            this.data = this.data.concat(data);

            this.part++;
        }
    }

    add(entry) {
        this.data.splice(0, 0, entry);
    }

    delete(id) {
        let index = this.data.findIndex(x => x.id == id);
        if (index != -1) {
            this.data.splice(index, 1);
        }
    }

    edit(id, text) {
        let index = this.data.findIndex(x => x.id == id);
        if (index != -1) {
            this.data[index].text = text;
        }
    }
}

var entries = new Entries();