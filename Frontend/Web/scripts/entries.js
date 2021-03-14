class Entries {
    data = [];

    constructor() {
        this.load();
    }

    async load() {
        const response = await fetch(`http://localhost:5000/api/entries`);
        this.data = await response.json();
        this.data.reverse();
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
}

var entries = new Entries();