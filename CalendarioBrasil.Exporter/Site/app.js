const lista = document.getElementById("calendarios");

async function carregarCalendarios() {
    try {
        const resposta = await fetch("/manifest.json", { cache: "no-store" });

        if (!resposta.ok) {
            throw new Error("Não foi possível consultar o manifesto.");
        }

        const manifesto = await resposta.json();

        if (!Array.isArray(manifesto.calendarios)) {
            throw new Error("Manifesto inválido.");
        }

        lista.replaceChildren();

        for (const calendario of manifesto.calendarios) {
            if (!calendario || !Number.isInteger(calendario.ano) || calendario.ano < 2024 || calendario.ano > 2100 || !/^[A-Z]{2}$/.test(calendario.uf) || !Number.isInteger(calendario.total)) {
                continue;
            }

            const escopo = calendario.escopo || "Geral";
            const caminho = calendario.arquivo;

            if (!["Geral", "ExecutivoEstadual", "JudiciarioEstadual"].includes(escopo) || typeof caminho !== "string" || !/^(?:[0-9]{4}\/[A-Z]{2}\.json|escopos\/(?:ExecutivoEstadual|JudiciarioEstadual)\/[0-9]{4}\/[A-Z]{2}\.json)$/.test(caminho)) {
                continue;
            }

            const item = document.createElement("li");
            const link = document.createElement("a");
            link.href = `/${caminho}`;
            link.textContent = `${calendario.ano} / ${calendario.uf} / ${escopo} — ${calendario.cobertura} (${calendario.total} eventos)`;
            item.appendChild(link);
            lista.appendChild(item);
        }

        if (lista.children.length === 0) {
            lista.textContent = "Nenhum calendário disponível.";
        }
    }
    catch {
        lista.textContent = "Não foi possível carregar a lista de calendários.";
    }
}

carregarCalendarios();