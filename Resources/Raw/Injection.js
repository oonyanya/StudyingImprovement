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

    function isHTML(htmlString) {
        let parser = new DOMParser();
        let doc = parser.parseFromString(htmlString, "application/xml");
        let errorNode = doc.querySelector('parsererror');
        return !errorNode;
    }

    function replace_element(element, f) {
        element.innerHTML = f(element.innerHTML);
    }

    function wash_string_for_sreen_reader(s) {
        /* EDGEのスクリーンリーダーが途中で止まってしまう対策 */
        return s.replaceAll("・", "、");
    }

    function add_folding_marker(elements) {
        const id = 0;
        elements.forEach((e) => {
            replace_element(e, (s) => {
                /* 折り畳み機能で暗記ノートの機能が破壊されるので実装しなおし　*/
                let new_s = s.
                    replaceAll(/<span class=\"span-memory\">(.+?)<\/span>/g, "<label class='span-memory-label'><input type='checkbox'></input><span class='span-memory'>$1</span></label>").
                    replaceAll(/（/g, '<label class=\'folding_box\'>※<input type=\'checkbox\'></input><span>（').
                    replaceAll(/）/g, '）</span></label>');
                return wash_string_for_sreen_reader(new_s);
            });
        });
    }

    function wash_element_for_screen_reader(elements) {
        elements.forEach((e) => {
            replace_element(e, (s) => {
                return wash_string_for_sreen_reader(s);
            });
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
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h2');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h3');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > p');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > table > tbody > tr > td');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ul > li');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ol > li');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#answer_box_on > div > div > div > div > p');
    add_folding_marker(elements);

    /* 暗記ノートの機能が破壊されてるので再度実装する */
    $('#span-memory-toggle').unbind('click').click(function () {
        if ($('#span-memory-toggle').prop('checked')) {
            $('.memory_box > input[type=checkbox]').prop('checked', false);
        } else {
            $('.memory_box > input[type=checkbox]').prop('checked', true);
        }
    });

}

if (document.readyState == 'loading') {
    document.addEventListener("DOMContentLoaded", () => {
        Inject_fn();
    });
} else {
    Inject_fn();
}