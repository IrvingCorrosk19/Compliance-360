window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  const STEPS = [
    { id: 1, role: "Regulatory Specialist", title: "Crear producto + expediente", action: "new-product" },
    { id: 2, role: "Regulatory Specialist", title: "Pedir docs fábrica", action: "ask-docs" },
    { id: 3, role: "Regulatory Specialist", title: "Marcar requisito recibido", action: "mark-received" },
    { id: 4, role: "Regulatory Specialist", title: "Armar expediente", action: "assemble" },
    { id: 5, role: "Regulatory Reviewer", title: "Rechazar un requisito", action: "reject-req" },
    { id: 6, role: "Regulatory Specialist", title: "Corregir y marcar de nuevo", action: "mark-received" },
    { id: 7, role: "Regulatory Reviewer", title: "Aceptar requisito", action: "accept-req" },
    { id: 8, role: "Regulatory Specialist", title: "Declarar técnicamente completo", action: "declare-ready" },
    { id: 9, role: "Regulatory Approver", title: "Aprobar internamente para sometimiento", action: "approve-internal" },
    { id: 10, role: "Regulatory Submitter", title: "Registrar sometimiento", action: "submit" },
    { id: 11, role: "Regulatory Manager", title: "Registrar observación de autoridad", action: "observe" },
    { id: 12, role: "Regulatory Specialist", title: "Responder observación", action: "respond-obs" },
    { id: 13, role: "Regulatory Submitter", title: "Resometimiento (estado Resubmitted / nuevo submit)", action: "submit" },
    { id: 14, role: "Regulatory Manager", title: "Registrar aprobación externa + CT/RS", action: "approve-ext" },
    { id: 15, role: "Regulatory Manager", title: "Ver CT/RS activo e iniciar renovación", action: "renewal" }
  ];

  function stateMachine() {
    return {
      status: "Planning",
      product: null,
      requirements: [{ id: "R1", name: "Literatura técnica", status: "Pending", critical: true }],
      observations: [],
      ctrs: null,
      log: []
    };
  }

  function render(root) {
    if (!root) return;
    let stepIdx = 0;
    let sim = stateMachine();
    const labels = (window.C360_MANUAL.data.workflow || {}).statusLabels || {};

    function paint() {
      const step = STEPS[stepIdx];
      root.innerHTML = `
        <div class="card" id="guided-sim">
          <div style="display:flex;justify-content:space-between;gap:1rem;flex-wrap:wrap;align-items:center">
            <div>
              <div class="pill" style="--role-color:#0ea5e9">Simulación guiada del expediente</div>
              <h2 style="margin:.4rem 0">Paso ${step.id} de ${STEPS.length}: ${step.title}</h2>
              <p class="muted"><strong>Ahora actúas como ${step.role}.</strong> En el sistema real cada rol usa una cuenta distinta (SoD).</p>
            </div>
            <div style="min-width:180px">
              <div class="muted" style="font-size:.8rem">Progreso</div>
              <div class="progress-ring"><span style="width:${Math.round((stepIdx/(STEPS.length-1))*100)}%"></span></div>
            </div>
          </div>
          <div class="flow" aria-label="Estado actual">
            <span class="step current">${labels[sim.status] || sim.status}</span>
            ${sim.ctrs ? `<span class="step">CT/RS: ${sim.ctrs}</span>` : ""}
          </div>
          <div class="sim-shell" style="margin-top:.75rem">
            <aside class="sim-nav" aria-label="Menú RA simulado">
              <div class="brand-title" style="margin-bottom:.5rem">Consola RA</div>
              ${["Dashboard","Portafolio","Pipeline","Expedientes","Registros CT/RS"].map((x,i)=>`<button type="button" class="${i===3?'active':''}">${x}</button>`).join("")}
            </aside>
            <div class="sim-body">
              <div class="sim-topbar">
                <div><strong>Expediente DEMO-001</strong><div class="muted">Tenant lab · Rol: ${step.role}</div></div>
                <span class="badge">${labels[sim.status] || sim.status}</span>
              </div>
              <ul class="list-tight">
                ${sim.requirements.map(r => `<li>${r.critical?'⚠ ':''}${r.name}: <strong>${r.status}</strong></li>`).join("")}
              </ul>
              <div class="ra-actions" style="display:flex;flex-wrap:wrap;gap:.4rem;margin-top:.8rem">
                <button type="button" class="btn primary" id="sim-do">${step.title}</button>
                <button type="button" class="btn" id="sim-next" ${stepIdx>=STEPS.length-1?"disabled":""}>Siguiente paso</button>
                <button type="button" class="btn ghost" id="sim-reset">Reiniciar simulación</button>
              </div>
              <p class="muted" style="margin-top:.8rem">Historial: ${sim.log.slice(-4).join(" → ") || "—"}</p>
            </div>
          </div>
        </div>`;
      root.querySelector("#sim-do").onclick = () => {
        apply(step);
        const p = window.C360_MANUAL.progress;
        const st = p.load();
        if (!st.completedSimSteps.includes(step.id)) st.completedSimSteps.push(step.id);
        p.save(st);
        window.C360_MANUAL.ui?.toast("Acción simulada: " + step.title, "success");
        paint();
      };
      root.querySelector("#sim-next").onclick = () => { if (stepIdx < STEPS.length - 1) { stepIdx++; paint(); } };
      root.querySelector("#sim-reset").onclick = () => { stepIdx = 0; sim = stateMachine(); paint(); };
    }

    function apply(step) {
      const map = {
        "new-product": () => { sim.product = "PRODUCTO DEMO"; sim.status = "Planning"; },
        "ask-docs": () => { sim.status = "WaitingManufacturerDocuments"; },
        "mark-received": () => { sim.requirements[0].status = "Received"; sim.status = "DocumentsReceived"; },
        "assemble": () => { sim.status = "Assembling"; },
        "reject-req": () => { sim.requirements[0].status = "Rejected"; },
        "accept-req": () => { sim.requirements[0].status = "Accepted"; },
        "declare-ready": () => { sim.status = "ReadyForSubmission"; },
        "approve-internal": () => { sim.status = "ApprovedForSubmission"; },
        "submit": () => { sim.status = sim.status === "CorrectingObservation" || sim.log.includes("observe") ? "Resubmitted" : "Submitted"; },
        "observe": () => { sim.status = "Observed"; sim.observations.push("Falta literatura"); },
        "respond-obs": () => { sim.status = "CorrectingObservation"; },
        "approve-ext": () => { sim.status = "Approved"; sim.ctrs = "MQ-4521-07-26"; },
        "renewal": () => { sim.log.push("Renovación iniciada"); }
      };
      (map[step.action] || (()=>{}))();
      sim.log.push(step.title);
    }

    paint();
  }

  window.C360_MANUAL.simulator = { STEPS, render };
})();
