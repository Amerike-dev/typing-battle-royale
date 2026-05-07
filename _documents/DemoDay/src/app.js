// =====================================================================
// Typing Battle Royale — Demo Day backlog (lógica)
// Render, modal, filtros, markdown, init.
// Lee TICKETS y TEAM de data.js, TICKET_STEPS de los archivos de olas.
// =====================================================================

const TYPE_LABEL = { bug: 'Bug', feature: 'Feature', tech: 'Tech Debt' };
const PRIORITY_LABEL = { critical: 'Crítica', high: 'Alta', medium: 'Media', low: 'Baja' };
const EFFORT_LABEL = { S: 'Effort: S (≤4h)', M: 'Effort: M (1-2 días)', L: 'Effort: L (3+ días)' };
const STATUS_LABEL = { todo: 'Pendiente', 'in-progress': 'En curso', done: 'Hecho' };
const STATUS_CLASS = { todo: 'status-todo', 'in-progress': 'status-progress', done: 'status-done' };

const state = { tab: 'active', type: 'all', priority: 'all', assignee: 'all', search: '' };

// ───────────── Helpers de datos ─────────────

function getReverseDeps(id) {
    return TICKETS.filter(t => (t.deps || []).includes(id));
}

function findById(id) {
    return TICKETS.find(t => t.id === id) || null;
}

function findMember(id) {
    return TEAM.find(m => m.id === id) || null;
}

function getInitials(name) {
    return name.split(/\s+/).map(s => s[0]).join('').slice(0, 2).toUpperCase();
}

function getSteps(id) {
    return (typeof TICKET_STEPS !== 'undefined' && TICKET_STEPS[id]) || '';
}

// Topological sort en olas. Retorna { waves, cycle }.
function topologicalWaves(tickets) {
    const remaining = tickets.slice();
    const placed = new Set();
    const waves = [];
    while (remaining.length) {
        const wave = remaining.filter(t =>
            (t.deps || []).every(d => placed.has(d) || !tickets.find(x => x.id === d))
        );
        if (!wave.length) return { waves, cycle: remaining };
        wave.forEach(t => placed.add(t.id));
        waves.push(wave);
        wave.forEach(t => {
            const idx = remaining.indexOf(t);
            if (idx !== -1) remaining.splice(idx, 1);
        });
    }
    return { waves, cycle: [] };
}

// ───────────── Markdown ─────────────

// Mini markdown renderer — soporta # headings, **bold**, *italic*, `code`,
// fenced code (```), listas - / 1., líneas en blanco como párrafo.
// [TBR-XXX] se renderiza como link clickeable al ticket.
function mdToHtml(md) {
    if (!md) return '';
    md = md.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    const lines = md.split('\n');
    let html = '';
    let inList = null;
    let inCode = false;
    let buffer = [];

    const flushP = () => {
        if (buffer.length) { html += `<p>${buffer.join(' ')}</p>`; buffer = []; }
    };
    const closeList = () => { if (inList) { html += `</${inList}>`; inList = null; } };
    const inline = (s) => s
        .replace(/`([^`]+)`/g, '<code>$1</code>')
        .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
        .replace(/\*([^*]+)\*/g, '<em>$1</em>')
        .replace(/\[([A-Z]+-\d+)\]/g, '<span class="dep-link" data-id="$1">$1</span>');

    for (let line of lines) {
        if (line.startsWith('```')) {
            flushP(); closeList();
            if (inCode) { html += '</code></pre>'; inCode = false; }
            else { html += '<pre><code>'; inCode = true; }
            continue;
        }
        if (inCode) { html += line + '\n'; continue; }
        if (line.startsWith('### ')) { flushP(); closeList(); html += `<h5 class="md-h">${inline(line.slice(4))}</h5>`; continue; }
        if (line.startsWith('## '))  { flushP(); closeList(); html += `<h4 class="md-h">${inline(line.slice(3))}</h4>`; continue; }
        const ulm = line.match(/^- (.*)/);
        if (ulm) { flushP(); if (inList !== 'ul') { closeList(); html += '<ul>'; inList = 'ul'; } html += `<li>${inline(ulm[1])}</li>`; continue; }
        const olm = line.match(/^\d+\. (.*)/);
        if (olm) { flushP(); if (inList !== 'ol') { closeList(); html += '<ol>'; inList = 'ol'; } html += `<li>${inline(olm[1])}</li>`; continue; }
        if (line.trim() === '') { flushP(); closeList(); continue; }
        if (inList) closeList();
        buffer.push(inline(line));
    }
    flushP(); closeList();
    if (inCode) html += '</code></pre>';
    return html;
}

// ───────────── Avatares ─────────────

function renderAvatar(assigneeId, sizeClass = '') {
    if (!assigneeId) {
        return `<span class="assignee unassigned" title="Sin asignar">
            <span class="avatar avatar-empty ${sizeClass}">?</span>
            <span class="assignee-name">Sin asignar</span>
        </span>`;
    }
    const m = findMember(assigneeId);
    if (!m) {
        return `<span class="assignee unknown" title="${assigneeId} (no está en TEAM)">
            <span class="avatar avatar-empty ${sizeClass}">!</span>
            <span class="assignee-name">${assigneeId}</span>
        </span>`;
    }
    return `<span class="assignee" title="${m.name}">
        <span class="avatar ${sizeClass}" style="background:${m.color}">${getInitials(m.name)}</span>
        <span class="assignee-name">${m.name}</span>
    </span>`;
}

// ───────────── Stats + filtros ─────────────

function renderStats() {
    const active = TICKETS.filter(t => t.status !== 'done');
    const counts = {
        total: TICKETS.length,
        done: TICKETS.filter(t => t.status === 'done').length,
        critical: active.filter(t => t.priority === 'critical').length,
        high: active.filter(t => t.priority === 'high').length,
        medium: active.filter(t => t.priority === 'medium').length,
        low: active.filter(t => t.priority === 'low').length
    };
    document.getElementById('stats').innerHTML = `
        <div class="stat-card"><div class="num">${counts.total}</div><div class="lbl">Total</div></div>
        <div class="stat-card low"><div class="num">${counts.done}</div><div class="lbl">Hechos</div></div>
        <div class="stat-card critical"><div class="num">${counts.critical}</div><div class="lbl">Crítica</div></div>
        <div class="stat-card high"><div class="num">${counts.high}</div><div class="lbl">Alta</div></div>
        <div class="stat-card medium"><div class="num">${counts.medium}</div><div class="lbl">Media</div></div>
    `;
    document.getElementById('tabCountActive').textContent = TICKETS.filter(t => t.status !== 'done').length;
    document.getElementById('tabCountDone').textContent = counts.done;
}

function applyFilters(list) {
    const q = state.search.trim().toLowerCase();
    return list.filter(t => {
        if (state.type !== 'all' && t.type !== state.type) return false;
        if (state.priority !== 'all' && t.priority !== state.priority) return false;
        if (state.assignee === 'unassigned' && t.assignee) return false;
        if (state.assignee !== 'all' && state.assignee !== 'unassigned' && t.assignee !== state.assignee) return false;
        if (q) {
            const member = findMember(t.assignee);
            const memberName = member ? member.name : '';
            const haystack = `${t.id} ${t.title} ${t.summary} ${memberName} ${(t.files || []).join(' ')}`.toLowerCase();
            if (!haystack.includes(q)) return false;
        }
        return true;
    });
}

// ───────────── Vistas ─────────────

function renderBoard() {
    const view = document.getElementById('view');
    if (state.tab === 'active') view.innerHTML = renderActiveView();
    else if (state.tab === 'done') view.innerHTML = renderDoneView();
    else if (state.tab === 'graph') view.innerHTML = renderGraphView();
    bindCardClicks();
}

function renderActiveView() {
    const active = TICKETS.filter(t => t.status !== 'done');
    const filtered = applyFilters(active);
    if (!filtered.length) return `<p class="empty-state">No hay tickets activos con esos filtros.</p>`;
    return `<div class="board">${filtered.map(renderCard).join('')}</div>`;
}

function renderCard(t) {
    return `
        <article class="ticket" data-id="${t.id}">
            <div class="ticket-head">
                <span class="ticket-id">${t.id}</span>
                <span class="priority-dot ${t.priority}" title="${PRIORITY_LABEL[t.priority]}"></span>
            </div>
            <h3>${t.title}</h3>
            <p>${t.summary}</p>
            <div class="badges">
                <span class="badge ${t.type}">${TYPE_LABEL[t.type]}</span>
                <span class="badge ${t.priority}">${PRIORITY_LABEL[t.priority]}</span>
                <span class="badge effort">${EFFORT_LABEL[t.effort]}</span>
                <span class="badge ${STATUS_CLASS[t.status]}">${STATUS_LABEL[t.status]}</span>
            </div>
            <div class="ticket-assignee">${renderAvatar(t.assignee)}</div>
        </article>
    `;
}

function renderDoneView() {
    const done = applyFilters(TICKETS.filter(t => t.status === 'done'));
    if (!done.length) return `<p class="empty-state">Aún no hay tickets completados. Cambiá <code>status: 'todo'</code> a <code>'done'</code> en src/data.js.</p>`;
    return `<div class="done-list">${done.map(t => `
        <div class="done-row" data-id="${t.id}">
            <span class="ticket-id">${t.id}</span>
            <h3>${t.title}</h3>
            <span class="badge ${t.type}">${TYPE_LABEL[t.type]}</span>
            ${renderAvatar(t.assignee)}
        </div>
    `).join('')}</div>`;
}

function renderGraphView() {
    const filtered = applyFilters(TICKETS);
    const { waves, cycle } = topologicalWaves(filtered);
    const help = `
        <div class="graph-help">
            <strong>Olas de ejecución:</strong> cada columna agrupa tickets cuyas dependencias ya están en olas anteriores.
            La <strong>Ola 1</strong> son tareas sin bloqueadores — empezá por ahí.
            Click en cualquier card para ver "Bloqueado por" y "Bloquea a".
        </div>`;
    if (!waves.length && !cycle.length) return `${help}<p class="empty-state">No hay tickets que coincidan con los filtros.</p>`;
    const wavesHtml = waves.map((wave, i) => `
        <div class="wave-column">
            <div class="wave-header">
                <span class="wave-title">Ola ${i + 1}</span>
                <span class="wave-count">${wave.length}</span>
            </div>
            ${wave.map(renderWaveCard).join('')}
        </div>
    `).join('');
    const cycleWarning = cycle.length ? `
        <div class="cycle-warning">
            ⚠️ Ciclo de dependencias detectado entre: ${cycle.map(t => `<strong>${t.id}</strong>`).join(', ')}.
            Revisá <code>deps</code> en src/data.js — ningún ticket puede depender (directa o indirectamente) de uno que dependa de él.
        </div>
    ` : '';
    return `${help}<div class="graph"><div class="waves-row">${wavesHtml}</div>${cycleWarning}</div>`;
}

function renderWaveCard(t) {
    const deps = (t.deps || []).length;
    const blocks = getReverseDeps(t.id).length;
    return `
        <div class="wave-card ${t.status === 'done' ? 'is-done' : ''}" data-id="${t.id}">
            <div class="wc-head">
                <span class="ticket-id">${t.id}</span>
                <span class="priority-dot ${t.priority}" title="${PRIORITY_LABEL[t.priority]}"></span>
            </div>
            <h4>${t.title}</h4>
            <div class="wave-meta">
                <span class="badge ${t.type}">${TYPE_LABEL[t.type]}</span>
                ${deps ? `<span>← ${deps} dep${deps > 1 ? 's' : ''}</span>` : ''}
                ${blocks ? `<span class="deps-count">→ bloquea ${blocks}</span>` : ''}
            </div>
        </div>
    `;
}

function bindCardClicks() {
    document.querySelectorAll('[data-id]').forEach(el => {
        el.addEventListener('click', () => openModal(el.dataset.id));
    });
}

// ───────────── Modal ─────────────

function openModal(id) {
    const t = TICKETS.find(x => x.id === id);
    if (!t) return;

    const filesHtml = (t.files || []).map(f => `<code>${f}</code>`).join('');
    const acceptanceHtml = (t.acceptance || []).map(a => `<li>${a}</li>`).join('');

    const renderDepItem = (depId) => {
        const d = findById(depId);
        if (!d) return `<div class="dep-item"><span class="dep-id">${depId}</span><span class="dep-title" style="color:var(--text-muted)">(no encontrado)</span></div>`;
        return `<div class="dep-item" data-id="${d.id}">
            <span class="priority-dot ${d.priority}"></span>
            <span class="dep-id">${d.id}</span>
            <span class="dep-title">${d.title}</span>
            <span class="badge ${STATUS_CLASS[d.status]}">${STATUS_LABEL[d.status]}</span>
        </div>`;
    };

    const blockedBy = (t.deps || []).map(renderDepItem).join('') || '<p class="no-deps">Sin bloqueadores — se puede empezar ya.</p>';
    const blocksList = getReverseDeps(t.id);
    const blocks = blocksList.length
        ? blocksList.map(b => renderDepItem(b.id)).join('')
        : '<p class="no-deps">No bloquea a otros tickets.</p>';

    const modalContent = document.getElementById('modalContent');
    modalContent.innerHTML = `
        <div class="modal-head">
            <div>
                <span class="ticket-id">${t.id}</span>
                <h2>${t.title}</h2>
                <div class="badges" style="margin-top:8px;">
                    <span class="badge ${t.type}">${TYPE_LABEL[t.type]}</span>
                    <span class="badge ${t.priority}">${PRIORITY_LABEL[t.priority]}</span>
                    <span class="badge effort">${EFFORT_LABEL[t.effort]}</span>
                    <span class="badge ${STATUS_CLASS[t.status]}">${STATUS_LABEL[t.status]}</span>
                </div>
            </div>
        </div>

        <div class="section-block">
            <h4>Asignado a</h4>
            <div class="assignee-row">
                ${renderAvatar(t.assignee, 'avatar-lg')}
                <span class="hint">edita <code>assignee</code> · <code>status</code> en src/data.js</span>
            </div>
        </div>

        <div class="section-block">
            <h4>Descripción</h4>
            <div class="md">${mdToHtml(t.summary)}</div>
        </div>

        <div class="section-block">
            <h4>Pasos para resolver</h4>
            <div class="md">${mdToHtml(getSteps(t.id)) || '<p class="no-deps">Sin walkthrough todavía.</p>'}</div>
        </div>

        <div class="section-block">
            <h4>Criterios de aceptación</h4>
            <ul>${acceptanceHtml}</ul>
        </div>

        <div class="section-block">
            <h4>Archivos afectados</h4>
            <div class="file-list">${filesHtml}</div>
        </div>

        <div class="section-block">
            <h4>Bloqueado por (${(t.deps || []).length})</h4>
            <div class="deps-list">${blockedBy}</div>
        </div>

        <div class="section-block">
            <h4>Bloquea a (${blocksList.length})</h4>
            <div class="deps-list">${blocks}</div>
        </div>
    `;

    modalContent.querySelectorAll('[data-id]').forEach(el => {
        el.addEventListener('click', (e) => { e.stopPropagation(); openModal(el.dataset.id); });
    });

    const dlg = document.getElementById('ticketModal');
    if (!dlg.open) dlg.showModal();
}

// ───────────── Bindings + init ─────────────

function renderAssigneeFilters() {
    const container = document.getElementById('assigneeFilters');
    if (!container) return;
    const baseChips = `
        <span class="filter-label">Asignado</span>
        <button class="chip active" data-value="all">Todos</button>
        <button class="chip" data-value="unassigned">Sin asignar</button>
    `;
    const memberChips = TEAM.map(m => `
        <button class="chip" data-value="${m.id}">
            <span class="chip-avatar" style="background:${m.color}">${getInitials(m.name)}</span>${m.name}
        </button>
    `).join('');
    container.innerHTML = baseChips + memberChips;
}

function bindFilters() {
    document.querySelectorAll('.filter-group[data-filter-key]').forEach(group => {
        const key = group.dataset.filterKey;
        group.addEventListener('click', e => {
            const chip = e.target.closest('.chip');
            if (!chip || !group.contains(chip)) return;
            group.querySelectorAll('.chip').forEach(c => c.classList.remove('active'));
            chip.classList.add('active');
            state[key] = chip.dataset.value;
            renderBoard();
        });
    });

    document.getElementById('search').addEventListener('input', e => {
        state.search = e.target.value;
        renderBoard();
    });

    document.getElementById('closeModal').addEventListener('click', () => {
        document.getElementById('ticketModal').close();
    });

    document.getElementById('ticketModal').addEventListener('click', e => {
        if (e.target.id === 'ticketModal') e.target.close();
    });
}

function bindTabs() {
    document.querySelectorAll('.tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            state.tab = tab.dataset.tab;
            renderBoard();
        });
    });
}

document.addEventListener('DOMContentLoaded', () => {
    renderStats();
    renderAssigneeFilters();
    renderBoard();
    bindFilters();
    bindTabs();
});
