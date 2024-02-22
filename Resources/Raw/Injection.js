function Inject_fn() {
    const MARU = 'O';
    const BATSU = 'X';

    function add_temp_marker(elements) {
        add_temp_marker(elements, null);
    }
    function add_temp_marker(elements, matching) {
        let id = 0;
        elements.forEach((e) => {
            if (e.textContent.match(matching) == null) {
                return;
            }
            const new_item1 = document.createElement('input');
            new_item1.className = 'temp_checkbox';
            new_item1.id = 'temp_checkbox' + id; /* ID must be uniq */
            new_item1.type = 'checkbox';

            const new_item2 = document.createElement('label');
            new_item2.setAttribute('for', new_item1.id);
            new_item2.textContent = MARU;

            id++;

            const new_item3 = document.createElement('input');
            new_item3.className = 'temp_checkbox';
            new_item3.id = 'temp_checkbox' + id; /* ID must be uniq */
            new_item3.type = 'checkbox';

            const new_item4 = document.createElement('label');
            new_item4.setAttribute('for', new_item3.id);
            new_item4.textContent = BATSU;

            id++;

            e.append(new_item1, new_item2, new_item3, new_item4);
        });
    }

    function add_folding_marker(elements) {
        const id = 0;
        elements.forEach((e) => {
            /* 暗記ノート用マーカーがある場所はマーカーが機能しなくなるので無視する */
            if (e.outerHTML.match("span-memory") == null) {
                const new_html = e.innerHTML.replaceAll(/（/g, '<label class=\'folding_box\'>※<input type=\'checkbox\'></input><span>（').replaceAll(/）/g, '）</span></label>');
                e.innerHTML = new_html;
            }
        });
    }

    function wash_text_for_screen_reader(elements) {
        elements.forEach((e) => {
             /* EDGEのスクリーンリーダーが途中で止まってしまう対策 */
             const new_html = e.innerHTML.replaceAll("・","、");
             e.innerHTML = new_html;
        });
    }

    let elements = document.querySelectorAll('div.question_text > div > ul > li');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > ol > li');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > table > tbody > tr > *:last-child');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > p');
    add_temp_marker(elements, /[^ァ-ヴ][ァ-ヴ]　/);

    elements = document.querySelectorAll('#doc > h1');
    wash_text_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h2');
    wash_text_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h3');
    wash_text_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > p');
    wash_text_for_screen_reader(elements);
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > table > tbody > tr > td');
    wash_text_for_screen_reader(elements);
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ul > li');
    wash_text_for_screen_reader(elements);
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ol > li');
    wash_text_for_screen_reader(elements);
    add_folding_marker(elements);

    elements = document.querySelectorAll('#answer_box_on > div > div > div > div > p');
    add_folding_marker(elements);
}

if (document.readyState == 'loading') {
    document.addEventListener("DOMContentLoaded",()=>{
        Inject_fn();
    });
} else {
    Inject_fn();
}
