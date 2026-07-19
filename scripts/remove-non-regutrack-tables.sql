-- Removes QMS / placeholder modules not required for REGUTRACK replacement.
-- Keeps: identity, tenant admin, audit_logs, storage, notifications, regulatory affairs.

SET search_path TO compliance360;

-- Enterprise placeholders
DROP TABLE IF EXISTS form_template_versions CASCADE;
DROP TABLE IF EXISTS form_templates CASCADE;
DROP TABLE IF EXISTS enterprise_workspace_items CASCADE;

-- Reporting
DROP TABLE IF EXISTS report_dashboard_bindings CASCADE;
DROP TABLE IF EXISTS report_exports CASCADE;
DROP TABLE IF EXISTS report_history CASCADE;
DROP TABLE IF EXISTS report_outputs CASCADE;
DROP TABLE IF EXISTS report_permissions CASCADE;
DROP TABLE IF EXISTS report_schedules CASCADE;
DROP TABLE IF EXISTS report_subscriptions CASCADE;
DROP TABLE IF EXISTS report_executions CASCADE;
DROP TABLE IF EXISTS report_parameters CASCADE;
DROP TABLE IF EXISTS report_templates CASCADE;
DROP TABLE IF EXISTS report_definitions CASCADE;
DROP TABLE IF EXISTS report_categories CASCADE;

-- Quality indicators
DROP TABLE IF EXISTS indicator_attachments CASCADE;
DROP TABLE IF EXISTS indicator_trends CASCADE;
DROP TABLE IF EXISTS indicator_alerts CASCADE;
DROP TABLE IF EXISTS indicator_processes CASCADE;
DROP TABLE IF EXISTS indicator_periods CASCADE;
DROP TABLE IF EXISTS indicator_results CASCADE;
DROP TABLE IF EXISTS indicator_thresholds CASCADE;
DROP TABLE IF EXISTS indicator_targets CASCADE;
DROP TABLE IF EXISTS indicator_measurements CASCADE;
DROP TABLE IF EXISTS indicator_formulas CASCADE;
DROP TABLE IF EXISTS indicator_history CASCADE;
DROP TABLE IF EXISTS quality_indicators CASCADE;
DROP TABLE IF EXISTS indicator_categories CASCADE;

-- Risk management
DROP TABLE IF EXISTS risk_attachments CASCADE;
DROP TABLE IF EXISTS risk_history CASCADE;
DROP TABLE IF EXISTS risk_indicators CASCADE;
DROP TABLE IF EXISTS risk_evidence CASCADE;
DROP TABLE IF EXISTS risk_reviews CASCADE;
DROP TABLE IF EXISTS risk_owners CASCADE;
DROP TABLE IF EXISTS risk_controls CASCADE;
DROP TABLE IF EXISTS risk_mitigation_plans CASCADE;
DROP TABLE IF EXISTS risk_treatments CASCADE;
DROP TABLE IF EXISTS risk_assessments CASCADE;
DROP TABLE IF EXISTS risks CASCADE;
DROP TABLE IF EXISTS risk_matrices CASCADE;
DROP TABLE IF EXISTS risk_categories CASCADE;

-- CAPA
DROP TABLE IF EXISTS capa_attachments CASCADE;
DROP TABLE IF EXISTS capa_history CASCADE;
DROP TABLE IF EXISTS capa_evidence CASCADE;
DROP TABLE IF EXISTS capa_effectiveness_checks CASCADE;
DROP TABLE IF EXISTS capa_preventive_actions CASCADE;
DROP TABLE IF EXISTS capa_corrective_actions CASCADE;
DROP TABLE IF EXISTS capa_containment_actions CASCADE;
DROP TABLE IF EXISTS capa_cause_analyses CASCADE;
DROP TABLE IF EXISTS capa_root_causes CASCADE;
DROP TABLE IF EXISTS capa_approvers CASCADE;
DROP TABLE IF EXISTS capa_owners CASCADE;
DROP TABLE IF EXISTS capas CASCADE;

-- Audit management (NOT audit_logs)
DROP TABLE IF EXISTS audit_corrective_action_links CASCADE;
DROP TABLE IF EXISTS audit_attachments CASCADE;
DROP TABLE IF EXISTS managed_audit_history CASCADE;
DROP TABLE IF EXISTS audit_recommendations CASCADE;
DROP TABLE IF EXISTS audit_non_conformities CASCADE;
DROP TABLE IF EXISTS audit_observations CASCADE;
DROP TABLE IF EXISTS audit_evidence CASCADE;
DROP TABLE IF EXISTS audit_findings CASCADE;
DROP TABLE IF EXISTS audit_areas CASCADE;
DROP TABLE IF EXISTS audit_auditors CASCADE;
DROP TABLE IF EXISTS audit_participants CASCADE;
DROP TABLE IF EXISTS audit_schedules CASCADE;
DROP TABLE IF EXISTS managed_audits CASCADE;
DROP TABLE IF EXISTS audit_checklist_items CASCADE;
DROP TABLE IF EXISTS audit_checklists CASCADE;
DROP TABLE IF EXISTS audit_plans CASCADE;
DROP TABLE IF EXISTS audit_programs CASCADE;

-- Workflows
DROP TABLE IF EXISTS workflow_notifications CASCADE;
DROP TABLE IF EXISTS workflow_escalations CASCADE;
DROP TABLE IF EXISTS workflow_history CASCADE;
DROP TABLE IF EXISTS workflow_assignments CASCADE;
DROP TABLE IF EXISTS workflow_instances CASCADE;
DROP TABLE IF EXISTS workflow_rules CASCADE;
DROP TABLE IF EXISTS workflow_transitions CASCADE;
DROP TABLE IF EXISTS workflow_steps CASCADE;
DROP TABLE IF EXISTS workflows CASCADE;

-- Document management
DROP TABLE IF EXISTS document_permissions CASCADE;
DROP TABLE IF EXISTS document_history CASCADE;
DROP TABLE IF EXISTS document_approvals CASCADE;
DROP TABLE IF EXISTS document_versions CASCADE;
DROP TABLE IF EXISTS documents CASCADE;
DROP TABLE IF EXISTS document_types CASCADE;
DROP TABLE IF EXISTS document_categories CASCADE;

-- Technical sheets
DROP TABLE IF EXISTS technical_sheet_approvals CASCADE;
DROP TABLE IF EXISTS technical_sheet_certifications CASCADE;
DROP TABLE IF EXISTS technical_sheet_nutrients CASCADE;
DROP TABLE IF EXISTS technical_sheet_ingredients CASCADE;
DROP TABLE IF EXISTS technical_sheet_versions CASCADE;
DROP TABLE IF EXISTS technical_sheets CASCADE;
DROP TABLE IF EXISTS products CASCADE;

-- Supplier management
DROP TABLE IF EXISTS supplier_expiration_alerts CASCADE;
DROP TABLE IF EXISTS supplier_evaluations CASCADE;
DROP TABLE IF EXISTS supplier_documents CASCADE;
DROP TABLE IF EXISTS suppliers CASCADE;
